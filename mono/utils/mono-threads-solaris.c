#include <config.h>

#if defined(__sun__) && defined(__svr4__)

#include <mono/utils/mono-threads.h>
#include <thread.h>
#include <signal.h>

void
mono_threads_platform_get_stack_bounds (guint8 **staddr, size_t *stsize)
{
	stack_t stack;

	*staddr = NULL;
	*stsize = (size_t)-1;

	if (thr_stksegment (&stack) == 0) {
		*staddr = (unsigned char *)stack.ss_sp - stack.ss_size;
		*stsize = stack.ss_size;
	}
}

#endif

